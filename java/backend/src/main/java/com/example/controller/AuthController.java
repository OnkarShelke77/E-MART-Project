package com.example.controller;

import com.example.dto.RegisterRequest;
import com.example.dto.LoginRequest;
import com.example.entity.User;
import com.example.service.UserService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import com.example.dto.LoginRequest;

@RestController
@RequestMapping("/auth")
@CrossOrigin(origins = "http://localhost:5173")
public class AuthController {

    private final UserService userService;

    public AuthController(UserService userService) {
        this.userService = userService;
    }

    // 🔥 REGISTER API
    @PostMapping("/register")
    public ResponseEntity<?> register(@RequestBody RegisterRequest request) {

        User savedUser = userService.registerUser(request);

        return ResponseEntity.ok(savedUser);
    }

    // 🔥🔥 LOGIN API (THIS WAS MISSING)
    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody LoginRequest request) {

        User user = userService.loginUser(
                request.getEmail(),
                request.getPassword());

        return ResponseEntity.ok(user);
    }
}
