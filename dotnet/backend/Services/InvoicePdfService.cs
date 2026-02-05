using System;
using System.Collections.Generic;
using System.IO;
using EMart.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EMart.Services
{
    public interface IInvoicePdfService
    {
        byte[] GenerateInvoicePdf(Ordermaster order, List<OrderItem> items);
    }

    public class InvoicePdfService : IInvoicePdfService
    {
        public byte[] GenerateInvoicePdf(Ordermaster order, List<OrderItem> items)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (items == null) throw new ArgumentNullException(nameof(items));

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 30, 30, 30, 30);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Fonts
                Font pageTitleFont = FontFactory.GetFont("Helvetica", 14, Font.BOLD);
                Font logoFont = FontFactory.GetFont("Helvetica", 20, Font.BOLD);
                Font normalFont = FontFactory.GetFont("Helvetica", 10);
                Font smallFont = FontFactory.GetFont("Helvetica", 9);
                Font boldFont = FontFactory.GetFont("Helvetica", 10, Font.BOLD);

                // 1) PAGE TITLE
                Paragraph invoiceTitle = new Paragraph("INVOICE PAGE", pageTitleFont);
                invoiceTitle.Alignment = Element.ALIGN_LEFT;
                document.Add(invoiceTitle);
                document.Add(new Paragraph(" "));

                // 2) HEADER
                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 25, 75 });

                PdfPCell logoCell = new PdfPCell(new Phrase("e-MART", logoFont));
                logoCell.FixedHeight = 70;
                logoCell.Border = Rectangle.NO_BORDER;
                logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                logoCell.HorizontalAlignment = Element.ALIGN_LEFT;

                PdfPCell bannerCell = new PdfPCell();
                bannerCell.FixedHeight = 70;
                bannerCell.Border = Rectangle.NO_BORDER;
                Paragraph p1 = new Paragraph("PREMIUM ONLINE SHOPPING", smallFont);
                p1.Alignment = Element.ALIGN_RIGHT;
                Paragraph p2 = new Paragraph("Official Invoice", boldFont);
                p2.Alignment = Element.ALIGN_RIGHT;
                bannerCell.AddElement(p1);
                bannerCell.AddElement(p2);

                header.AddCell(logoCell);
                header.AddCell(bannerCell);
                document.Add(header);
                document.Add(new Paragraph(" "));

                // 3) CATEGORY MENU (Static like Java)
                PdfPTable menu1 = new PdfPTable(6);
                menu1.WidthPercentage = 100;
                string[] row1 = { "HOME", "Apparel & Jewellery", "Appliances", "Books & Magazines", "Electronics", "Cameras" };
                foreach (var c in row1) menu1.AddCell(CreateSimpleCell(c, smallFont, Element.ALIGN_CENTER));
                document.Add(menu1);

                PdfPTable menu2 = new PdfPTable(6);
                menu2.WidthPercentage = 100;
                string[] row2 = { "Movie CDs", "Music", "Health & Beauty", "Lightings", "Furnishing", "Kitchen & Bath" };
                foreach (var c in row2) menu2.AddCell(CreateSimpleCell(c, smallFont, Element.ALIGN_CENTER));
                document.Add(menu2);
                document.Add(new Paragraph(" "));

                // 4) CUSTOMER INFO
                PdfPTable addressTable = new PdfPTable(1);
                addressTable.WidthPercentage = 100;
                PdfPCell addressCell = new PdfPCell();
                addressCell.Padding = 8;
                addressCell.AddElement(new Paragraph("Name : " + (order.User?.FullName ?? "N/A"), boldFont));
                addressCell.AddElement(new Paragraph("Address : " + (order.User?.Address ?? "Not provided"), normalFont));
                addressTable.AddCell(addressCell);
                document.Add(addressTable);
                document.Add(new Paragraph(" "));

                // 5) ORDER INFO
                document.Add(new Paragraph($"Order ID: {order.Id}", normalFont));
                document.Add(new Paragraph($"Order Date: {order.OrderDate:dd-MMM-yyyy HH:mm}", normalFont));
                document.Add(new Paragraph($"Order Status: {order.OrderStatus}", normalFont));
                document.Add(new Paragraph($"Payment Mode: {order.PaymentMode}", normalFont));
                document.Add(new Paragraph(" "));

                // 6) ITEMS TABLE
                PdfPTable itemTable = new PdfPTable(6);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 10, 35, 8, 15, 15, 17 });

                string[] headers = { "Item Cd.", "Item Desc.", "Qty", "List price", "eMcard price", "Amount" };
                foreach (var h in headers) itemTable.AddCell(CreateHeaderCell(h));

                decimal total = 0, savings = 0;
                int pointsEarned = 0;

                foreach (var item in items)
                {
                    var p = item.Product;
                    if (p == null) continue;

                    var listPrice = p.MrpPrice ?? 0;
                    var emcardPrice = p.CardholderPrice ?? listPrice;
                    var qty = item.Quantity;
                    var amount = emcardPrice * qty;
                    var save = (listPrice - emcardPrice) * qty;

                    total += amount;
                    savings += save;
                    pointsEarned += (p.PointsToBeRedeem ?? 0) * qty;

                    itemTable.AddCell(CreateNormalCell($"P{p.Id}"));
                    itemTable.AddCell(CreateNormalCell(p.ProdName));
                    itemTable.AddCell(CreateNormalCell(qty.ToString(), Element.ALIGN_CENTER));
                    itemTable.AddCell(CreateNormalCell(listPrice.ToString("F2"), Element.ALIGN_RIGHT));
                    itemTable.AddCell(CreateNormalCell(emcardPrice.ToString("F2"), Element.ALIGN_RIGHT));
                    itemTable.AddCell(CreateNormalCell(amount.ToString("F2"), Element.ALIGN_RIGHT));
                }
                document.Add(itemTable);
                document.Add(new Paragraph(" "));

                // 7) TOTAL SECTION
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 60, 40 });

                totalTable.AddCell(CreateHeaderCell("TOTAL"));
                totalTable.AddCell(CreateNormalCell(total.ToString("F2"), Element.ALIGN_RIGHT));
                totalTable.AddCell(CreateNormalCell("e-Points earned"));
                totalTable.AddCell(CreateNormalCell(pointsEarned.ToString(), Element.ALIGN_RIGHT));
                totalTable.AddCell(CreateNormalCell("You save:"));
                totalTable.AddCell(CreateNormalCell(savings.ToString("F2"), Element.ALIGN_RIGHT));
                document.Add(totalTable);
                document.Add(new Paragraph(" "));

                // 8) e-Point Account Info
                document.Add(new Paragraph("Your e-Point A/c:", boldFont));
                PdfPTable epointTable = new PdfPTable(2);
                epointTable.WidthPercentage = 40;
                epointTable.SetWidths(new float[] { 60, 40 });
                epointTable.AddCell(CreateNormalCell("OP Bal:")); epointTable.AddCell(CreateNormalCell("0", Element.ALIGN_RIGHT));
                epointTable.AddCell(CreateNormalCell("Tot Credited:")); epointTable.AddCell(CreateNormalCell(pointsEarned.ToString(), Element.ALIGN_RIGHT));
                epointTable.AddCell(CreateNormalCell("Redeemed today:")); epointTable.AddCell(CreateNormalCell("0", Element.ALIGN_RIGHT));
                epointTable.AddCell(CreateNormalCell("CL Bal:")); epointTable.AddCell(CreateNormalCell(pointsEarned.ToString(), Element.ALIGN_RIGHT));
                document.Add(epointTable);

                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Thank you for shopping with e-MART!", normalFont));

                document.Close();
                return ms.ToArray();
            }
        }

        private PdfPCell CreateHeaderCell(string text)
        {
            Font font = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.White);
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = BaseColor.DarkGray;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            return cell;
        }

        private PdfPCell CreateNormalCell(string text, int alignment = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", FontFactory.GetFont("Helvetica", 9)));
            cell.HorizontalAlignment = alignment;
            cell.Padding = 5;
            return cell;
        }

        private PdfPCell CreateSimpleCell(string text, Font font, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = alignment;
            cell.Padding = 5;
            return cell;
        }
    }
}
