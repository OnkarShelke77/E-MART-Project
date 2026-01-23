package com.example.controller;

import com.example.entity.User;
import com.example.service.UserService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/users")
public class UserController {

    private final UserService userService;

    public UserController(UserService userService) {
        this.userService = userService;
    }

    // CREATE
    @PostMapping
    public User createUser(@RequestBody User user) {
        return userService.saveUser(user);
    }

    // READ ALL
    @GetMapping
    public List<User> getAllUsers() {
        return userService.getAllUsers();
    }

    // READ BY ID
    @GetMapping("/{id}")
    public User getUserById(@PathVariable Integer id) {
        return userService.getUserById(id);
    }


    @DeleteMapping("/{id}")
    public ResponseEntity<Map<String, Object>> deleteUser(@PathVariable Integer id) {
        userService.deleteUser(id);

        Map<String, Object> response = new HashMap<>();
        response.put("message", "User deleted successfully");
        response.put("userId", id);
        response.put("timestamp", LocalDateTime.now());

        return ResponseEntity.ok(response);
    }


    @PutMapping("/{id}")
    public User updateUser(@PathVariable Integer id, @RequestBody User updatedUser) {
        return userService.updateUser(id, updatedUser);
    }
}