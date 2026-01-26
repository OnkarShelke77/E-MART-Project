package com.example.service;

import com.example.dto.RegisterRequest;
import com.example.entity.User;
import com.example.exception.GlobalExceptionHandler;
import com.example.repository.UserRepository;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class UserServiceImpl implements UserService {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder = new BCryptPasswordEncoder();

    public UserServiceImpl(UserRepository userRepository) {
        this.userRepository = userRepository;
    }

    // 🔹 Check duplicate email & mobile
    private void validateUniqueFields(User user) {

        userRepository.findByEmail(user.getEmail())
                .ifPresent(u -> {
                    throw new GlobalExceptionHandler.DuplicateFieldException(
                            "Email already exists: " + user.getEmail());
                });

        userRepository.findByMobile(user.getMobile())
                .ifPresent(u -> {
                    throw new GlobalExceptionHandler.DuplicateFieldException(
                            "Mobile number already exists: " + user.getMobile());
                });
    }

    // ---------------- NORMAL CRUD ----------------

    @Override
    public User saveUser(User user) {
        validateUniqueFields(user);
        return userRepository.save(user);
    }

    @Override
    public List<User> getAllUsers() {
        return userRepository.findAll();
    }

    @Override
    public User getUserById(Integer id) {
        return userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
    }

    @Override
    public void deleteUser(Integer id) {
        User user = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));

        userRepository.delete(user); // 🔥 delete only once
    }

    @Override
    public User updateUser(Integer id, User updatedUser) {

        User existingUser = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));

        existingUser.setFullName(updatedUser.getFullName());
        existingUser.setEmail(updatedUser.getEmail());
        existingUser.setMobile(updatedUser.getMobile());
        existingUser.setAddress(updatedUser.getAddress());

        return userRepository.save(existingUser);
    }

    // ---------------- 🔥 REGISTER USER ----------------

    @Override
    public User registerUser(RegisterRequest request) {

        User user = new User();

        user.setFullName(request.getFullName());
        user.setEmail(request.getEmail());
        user.setMobile(request.getMobile());
        user.setAddress(request.getAddress());

        // 🔐 HASH PASSWORD
        String hashedPassword = passwordEncoder.encode(request.getPassword());
        user.setPasswordHash(hashedPassword);

        // check duplicates
        validateUniqueFields(user);

        return userRepository.save(user);
    }

    // ---------------- 🔐 LOGIN USER ----------------

    @Override
    public User loginUser(String email, String password) {

        // find user by email
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("Invalid email or password"));

        // 🔐 check password with BCrypt
        if (!passwordEncoder.matches(password, user.getPasswordHash())) {
            throw new RuntimeException("Invalid email or password");
        }

        // 🎉 login success
        return user;
    }
}
