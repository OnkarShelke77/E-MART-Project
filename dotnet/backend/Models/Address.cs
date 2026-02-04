using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EMart.Models
{
    [Table("address")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("address_id")]
        public int AddressId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [Column("full_name")]
        [MaxLength(255)]
        public string? FullName { get; set; }

        [Column("mobile")]
        [MaxLength(255)]
        public string? Mobile { get; set; }

        [Column("house_no")]
        [MaxLength(255)]
        public string? HouseNo { get; set; }

        [Column("street")]
        [MaxLength(255)]
        public string? Street { get; set; }

        [Column("city")]
        [MaxLength(255)]
        public string? City { get; set; }

        [Column("state")]
        [MaxLength(255)]
        public string? State { get; set; }

        [Column("pincode")]
        [MaxLength(255)]
        public string? Pincode { get; set; }

        [Column("is_default")]
        [MaxLength(255)]
        public string? IsDefault { get; set; } = "N";
    }
}
