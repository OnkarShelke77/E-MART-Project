package com.example.service;

import com.example.entity.User;
import com.example.exception.GlobalExceptionHandler;
import com.example.repository.UserRepository;
import org.springframework.stereotype.Service;
import com.example.dto.RegisterRequest;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;

import java.util.List;

@Service
class UserServiceImpl implements UserService {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder = new BCryptPasswordEncoder();

    public UserServiceImpl(UserRepository userRepository) {
        this.userRepository = userRepository;
    }

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
        userRepository.deleteById(id);
        userRepository.delete(user);
    }

    @Override
    public User updateUser(Integer id, User updatedUser) {

        User existingUser = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));

        // Update allowed fields only
        existingUser.setFullName(updatedUser.getFullName());
        existingUser.setEmail(updatedUser.getEmail());
        existingUser.setMobile(updatedUser.getMobile());
        existingUser.setAddress(updatedUser.getAddress());

        return userRepository.save(existingUser);
    }

    public User registerUser(RegisterRequest request) {

        User user = new User();

        user.setFullName(request.getFullName());
        user.setEmail(request.getEmail());
        user.setMobile(request.getMobile());
        user.setAddress(request.getAddress());

        // 🔐 HASH THE PASSWORD
        String hashedPassword = passwordEncoder.encode(request.getPassword());
        user.setPasswordHash(hashedPassword);

        // check duplicate email & mobile
        validateUniqueFields(user);

        // save to DB
        return userRepository.save(user);
    }
}
