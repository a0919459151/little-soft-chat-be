# LittleSoftChat Gateway API Documentation

## Overview

The LittleSoftChat Gateway API serves as the single entry point for all client applications to interact with the microservices architecture. This gateway consolidates all REST API endpoints and handles authentication, authorization, and routing to the appropriate microservices.

## Base URL

```
Development: https://localhost:7001
Production: [To be configured]
```

## Authentication

The API uses JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## API Endpoints

### Authentication Endpoints

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "guid",
    "username": "string",
    "email": "string",
    "fullName": "string"
  }
}
```

#### POST /api/auth/login
Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "jwt-token-string",
    "refreshToken": "refresh-token-string",
    "user": {
      "userId": "guid",
      "username": "string",
      "email": "string",
      "fullName": "string"
    }
  }
}
```

#### POST /api/auth/refresh
Refresh JWT token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

#### POST /api/auth/logout
Logout user and invalidate token.

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

### Chat Endpoints

#### GET /api/chat/conversations
Get all conversations for the authenticated user.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "conversationId": "guid",
      "name": "string",
      "type": "Private|Group",
      "participants": [
        {
          "userId": "guid",
          "username": "string",
          "fullName": "string"
        }
      ],
      "lastMessage": {
        "messageId": "guid",
        "content": "string",
        "sentAt": "datetime",
        "senderName": "string"
      },
      "createdAt": "datetime"
    }
  ]
}
```

#### POST /api/chat/conversations
Create a new conversation.

**Request Body:**
```json
{
  "name": "string",
  "type": "Private|Group",
  "participantIds": ["guid1", "guid2"]
}
```

#### GET /api/chat/conversations/{conversationId}/messages
Get messages for a specific conversation.

**Query Parameters:**
- `page`: int (default: 1)
- `pageSize`: int (default: 50)

**Response:**
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "messageId": "guid",
        "conversationId": "guid",
        "senderId": "guid",
        "senderName": "string",
        "content": "string",
        "messageType": "Text|Image|File",
        "sentAt": "datetime",
        "isEdited": "boolean",
        "editedAt": "datetime?"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalItems": 250,
      "pageSize": 50
    }
  }
}
```

#### POST /api/chat/conversations/{conversationId}/messages
Send a message to a conversation.

**Request Body:**
```json
{
  "content": "string",
  "messageType": "Text|Image|File"
}
```

#### PUT /api/chat/messages/{messageId}
Edit a message.

**Request Body:**
```json
{
  "content": "string"
}
```

#### DELETE /api/chat/messages/{messageId}
Delete a message.

#### POST /api/chat/conversations/{conversationId}/participants
Add participants to a group conversation.

**Request Body:**
```json
{
  "participantIds": ["guid1", "guid2"]
}
```

#### DELETE /api/chat/conversations/{conversationId}/participants/{userId}
Remove a participant from a group conversation.

### Friends Endpoints

#### GET /api/friends
Get the authenticated user's friends list.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "friendId": "guid",
      "username": "string",
      "fullName": "string",
      "status": "Online|Offline|Away",
      "friendshipDate": "datetime"
    }
  ]
}
```

#### GET /api/friends/requests
Get pending friend requests.

**Response:**
```json
{
  "success": true,
  "data": {
    "sent": [
      {
        "requestId": "guid",
        "recipientId": "guid",
        "recipientUsername": "string",
        "sentAt": "datetime"
      }
    ],
    "received": [
      {
        "requestId": "guid",
        "senderId": "guid",
        "senderUsername": "string",
        "sentAt": "datetime"
      }
    ]
  }
}
```

#### POST /api/friends/requests
Send a friend request.

**Request Body:**
```json
{
  "recipientUsername": "string"
}
```

#### PUT /api/friends/requests/{requestId}/accept
Accept a friend request.

#### PUT /api/friends/requests/{requestId}/decline
Decline a friend request.

#### DELETE /api/friends/{friendId}
Remove a friend.

#### GET /api/friends/search
Search for users to add as friends.

**Query Parameters:**
- `query`: string (username or full name)

### Notifications Endpoints

#### GET /api/notifications
Get notifications for the authenticated user.

**Query Parameters:**
- `page`: int (default: 1)
- `pageSize`: int (default: 20)
- `unreadOnly`: boolean (default: false)

**Response:**
```json
{
  "success": true,
  "data": {
    "notifications": [
      {
        "notificationId": "guid",
        "type": "FriendRequest|Message|System",
        "title": "string",
        "content": "string",
        "isRead": "boolean",
        "createdAt": "datetime",
        "data": {}
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 3,
      "totalItems": 45,
      "pageSize": 20
    }
  }
}
```

#### PUT /api/notifications/{notificationId}/read
Mark a notification as read.

#### PUT /api/notifications/mark-all-read
Mark all notifications as read.

#### DELETE /api/notifications/{notificationId}
Delete a notification.

## Real-time Communication

The application uses SignalR for real-time communication. The SignalR hub is available at:

```
/notificationHub
```

### SignalR Events

#### Client to Server
- `JoinUserGroup`: Join user-specific notification group
- `LeaveUserGroup`: Leave user-specific notification group

#### Server to Client
- `ReceiveNotification`: Receive real-time notifications
- `MessageReceived`: Receive new chat messages
- `FriendRequestReceived`: Receive friend request notifications
- `UserStatusChanged`: Receive user online/offline status updates

## Error Handling

All endpoints return a consistent error response format:

```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    {
      "field": "fieldName",
      "message": "Field-specific error message"
    }
  ]
}
```

### HTTP Status Codes

- `200 OK`: Request successful
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate username)
- `500 Internal Server Error`: Server error

## Rate Limiting

API endpoints are rate-limited to prevent abuse:
- Authentication endpoints: 5 requests per minute
- General endpoints: 100 requests per minute
- Real-time endpoints: 500 requests per minute

## CORS Policy

The API accepts requests from the following origins:
- Development: `http://localhost:3000`, `http://localhost:3001`
- Production: [To be configured]

## WebSocket Connection

For real-time features, establish a WebSocket connection to the SignalR hub:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

// Start connection
await connection.start();

// Join user group for notifications
await connection.invoke("JoinUserGroup");

// Listen for notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
});
```

## SDK Examples

### JavaScript/TypeScript

```typescript
// Authentication
const loginResponse = await fetch('/api/auth/login', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        username: 'john_doe',
        password: 'password123'
    })
});

const loginData = await loginResponse.json();
const token = loginData.data.token;

// Get conversations
const conversationsResponse = await fetch('/api/chat/conversations', {
    headers: {
        'Authorization': `Bearer ${token}`
    }
});

const conversations = await conversationsResponse.json();
```

### C# (.NET)

```csharp
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://localhost:7001");

// Login
var loginRequest = new
{
    username = "john_doe",
    password = "password123"
};

var loginResponse = await httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

// Set authorization header
httpClient.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data.Token);

// Get conversations
var conversationsResponse = await httpClient.GetAsync("/api/chat/conversations");
var conversations = await conversationsResponse.Content.ReadFromJsonAsync<ApiResponse<List<ConversationDto>>>();
```

## Migration Guide

If you were previously calling microservices directly, update your API calls to use the Gateway:

### Before (Direct microservice calls)
```
POST https://localhost:7101/api/auth/login
GET https://localhost:7102/api/messages
GET https://localhost:7103/api/notifications
```

### After (Gateway calls)
```
POST https://localhost:7001/api/auth/login
GET https://localhost:7001/api/chat/conversations/{id}/messages
GET https://localhost:7001/api/notifications
```

## Support

For API support and questions, please contact the development team or create an issue in the project repository.
