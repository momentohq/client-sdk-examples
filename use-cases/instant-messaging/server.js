const http = require('http');
const socketIO = require('socket.io');
const dotenv = require('dotenv');
const { CacheClient, Configurations, CredentialProvider } = require('@gomomento/sdk');

dotenv.config();

const server = http.createServer();
const io = socketIO(server, { cors: { origin: '*', methods: ['GET', 'POST'] } });
const cacheClient = new CacheClient({
  configuration: Configurations.Laptop.latest(),
  credentialProvider: CredentialProvider.fromEnvironmentVariable({ environmentVariableName: 'AUTH_TOKEN' }),
  defaultTtlSeconds: 3600 // 1 hour chat history
})

io.on('connection', (socket) => {
  // User joined a chat room
  socket.on('join', async ({ room }) => {
    socket.join(room);

    let chatHistory = [];
    const response = await cacheClient.listFetch('chat', room);
    if (!response.is_miss) {
      chatHistory = response.valueListString().map(m => JSON.parse(m));
    }

    socket.emit('joined', { chatHistory });
  });

  // Message handler
  socket.on('message', async ({ room, message }) => {
    const chatMessage = JSON.stringify({ username: socket.id, message });

    // Broadcast the message to all connected clients in the room, including the person who sent it
    io.to(room).emit('message', { chatMessage });

    await cacheClient.listPushBack('chat', room, chatMessage);
  });

  // Leave a chat room
  socket.on('leave', ({ room }) => {
    socket.leave(room);
  });

  socket.on('disconnect', () => {
    console.log('User disconnected:', socket.id);
  });
});

server.listen(3000, () => {
  console.log(`Server running on port 3000`);
});