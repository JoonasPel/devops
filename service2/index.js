const express = require("express");
const http = require("http");
const cors = require("cors");

// Nodejs server confs
const PORT = 3001;
const app = express();
app.use(cors({
  origin: ["http://localhost:3000"],
  methods: ["GET", "POST"],
}));
app.use(express.json());

// Listen for requests
app.get("/", (req, res) => {
  console.log("service2 received a request!", req.body);
  res.send("Hi from service2");
});

// Start server
http.createServer(app).listen(PORT);
console.log("service2 running and listening for port", PORT);