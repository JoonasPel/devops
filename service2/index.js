const express = require("express");
const http = require("http");
const cors = require("cors");
const utils = require('./utils/utils.js');

// Nodejs server confs
const PORT = 8000;
const app = express();
app.use(cors({
  origin: ["http://localhost:3000"],
  methods: ["GET", "POST"],
}));
app.use(express.json());

// Listen for requests
app.get("/", (req, res) => {
  const text = req?.body?.text;
  if (text === 'STOP') { res.send(); process.exit(0); } 
  const socket = req?.socket;
  const newText =
  `${req?.body?.text} ${socket?.remoteAddress}:${socket?.remotePort}`;
  utils.writeToLogFile(newText);
  res.send();
});

// After delay init log file and start server
new Promise(resolve => {
  setTimeout(resolve, 2000);
}).then(() => {
  utils.createOrClearLogFile();
  http.createServer(app).listen(PORT);
});
