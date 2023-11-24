const http = require('http');

const PORT = 3000;

const server = http.createServer((req, res) => {
  res.writeHead(200);
  res.end("Hello Joonas\n");
});

server.listen(PORT, () => {
  console.log("server up and running");
});
