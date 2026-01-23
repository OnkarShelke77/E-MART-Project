package com.example.service;

import com.example.entity.*;
import com.example.exception.GlobalExceptionHandler;
import com.example.repository.*;
import com.example.service.CartService;
import jakarta.transaction.Transactional;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
@Transactional
public class CartServiceImpl implements CartService {

    @Autowired
    private CartRepository cartRepository;

    @Autowired
    private CartItemRepository cartItemRepository;

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private ProductRepository productRepository;

//    @Override
//    public void addToCart(Integer userId, Integer productId, Integer quantity) {
//
//        if (quantity <= 0) {
//            throw new IllegalArgumentException("Quantity must be greater than zero");
//        }
//
//        // 1. Get User
//        User user = userRepository.findById(userId)
//                .orElseThrow(() -> new RuntimeException("User not found"));
//
//        // 2. Get or create ONE cart
//        Cart cart = user.getCart();
//        if (cart == null) {
//            cart = new Cart();
//            cart.setUser(user);
//            cart.setIsActive('Y');
//            user.setCart(cart);           // bidirectional link
//            cartRepository.save(cart);    // or userRepository.save(user)
//        }
//
//        // 3. Get Product
//        Product product = productRepository.findById(productId)
//                .orElseThrow(() -> new RuntimeException("Product not found"));
//
//        // 4. Add / update item
//        Cartitem item = cartItemRepository
//                .findByCart_IdAndProd_Id(cart.getId(), productId)
//                .orElse(null);
//
//        if (item != null) {
//            item.setQuantity(item.getQuantity() + quantity);
//        } else {
//            Cartitem newItem = new Cartitem();
//            newItem.setCart(cart);
//            newItem.setProd(product);
//            newItem.setQuantity(quantity);
//            newItem.setPriceSnapshot(product.getMrpPrice());
//            cartItemRepository.save(newItem);
//        }
//    }


    @Override
    @Transactional
    public void addToCart(Integer userId, Integer productId, Integer quantity) {

        if (quantity == null || quantity <= 0) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }

        // 1. Fetch User
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));

        // 2. Get or create Cart (ONE cart per user)
        Cart cart = user.getCart();
        if (cart == null) {
            cart = new Cart();
            cart.setUser(user);
            cart.setIsActive('Y');
            user.setCart(cart);        // bidirectional link
            cartRepository.save(cart);
        }

        // 3. Fetch Product
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new RuntimeException("Product not found"));

        // 4. Check if product already exists in cart
        Optional<Cartitem> existingItem =
                cartItemRepository.findByCartIdAndProdId(cart.getId(), productId);

        if (existingItem.isPresent()) {
            // ✅ SAME PRODUCT → UPDATE QUANTITY
            Cartitem item = existingItem.get();
            item.setQuantity(item.getQuantity() + quantity);
            cartItemRepository.save(item); // explicit update (clear & safe)

        } else {
            // ✅ NEW PRODUCT → CREATE CART ITEM
            Cartitem newItem = new Cartitem();
            newItem.setCart(cart);
            newItem.setProd(product);
            newItem.setQuantity(quantity);
            newItem.setPriceSnapshot(product.getMrpPrice());

            cartItemRepository.save(newItem);
        }
    }


    @Override
    public void removeFromCart(Integer userId, Integer productId) {


        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));

        Cart cart = user.getCart();
        if (cart == null) {
            throw new RuntimeException("Cart not found");
        }

        Cartitem item = cartItemRepository
                .findByCart_IdAndProd_Id(cart.getId(), productId)
                .orElseThrow(() -> new RuntimeException("Item not found"));

        cartItemRepository.delete(item);
    }

    @Override
    public void viewCart(Integer userId) {
//        // Later convert to DTO
        Cart cart = cartRepository
                .findByUser_IdAndIsActive(userId, 'Y')
                .orElseThrow(() -> new RuntimeException("Cart not found"));

        cartItemRepository.findByCart_Id(cart.getId())
                .forEach(i ->
                        System.out.println(
                                i.getProd().getProdName() +
                                " x " + i.getQuantity() +
                                " = " + i.getPriceSnapshot()
                        )
                );
    }

    @Transactional
    public void deleteUser(Integer userId) {
        userRepository.deleteById(userId);
    }
}
