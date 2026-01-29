package com.example.service;

import com.example.entity.User;
import com.example.repository.UserRepository;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Optional;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class UserServiceTest {

    @Mock
    private UserRepository userRepository;

    @InjectMocks
    private UserServiceImpl userService;

    @Test
    void testGetUserById() {

        // Arrange (fake user)
        User user = new User();
        user.setId(1);
        user.setFullName("Pratik");
        user.setEmail("pratik@test.com");
        user.setProvider("LOCAL");

        when(userRepository.findById(1))
                .thenReturn(Optional.of(user));

        // Act
        User result = userService.getUserById(1);

        // Assert
        assertEquals("Pratik", result.getFullName());
    }
}
