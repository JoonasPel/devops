import dns from 'dns';
import express from 'express';
import {
  sleep,
  sendRequest,
  connectToRabbit,
  initRabbit,
  sendToRabbit,
  service2name,
  service2Port,
  rabbitMessageTopic,
  rabbitLogTopic,
  closeGracefully,
}
  from './utils/utils.mjs';

const States = {
  Running: "RUNNING",
  Paused: "PAUSED",
  Init: "INIT",
}
let currentState = States.Init;
let i = 0;  // number in front of logs

await connectToRabbit();
await initRabbit();
process.on('SIGTERM', closeGracefully);
let service2Address;
dns.lookup(service2name, (error, address) => {
  if (!error) { service2Address = address; }
  startServer();
  currentState = States.Running;
  startLogger();
});

const startLogger = async () => {
  let logging = true;
  process.on('stopLogging', () => {
    logging = false;
  });
  while (logging) {
    i++;
    const date = new Date();
    const text = `SND ${i} ${date.toISOString()} ${service2Address}:${service2Port}`;
    sendToRabbit(text, rabbitMessageTopic, 'hi.message');
    const statusCode = await sendRequest(text) ?? 500;
    const newText = `${statusCode.toString()} ${new Date().toISOString()}`;
    sendToRabbit(newText, rabbitLogTopic, 'hi.log');
    await sleep(2000);
  }
};

// listens for state change orders from APIgateway
const startServer = async () => {
  const app = express();
  app.use(express.text());
  app.put('/', (req, res) => {
    let newState = req.body;
    res.status(200); // changed to 400 if given state doesn't exist
    switch (newState) {
      case States.Paused:
        process.emit('stopLogging');
        currentState = States.Paused;
        break;
      case States.Running:
        // stop logging here too to counter problem of starting multiple loggers
        // by giving the state RUNNING multiple times in a row
        process.emit('stopLogging');
        currentState = States.Running;
        startLogger();  // continues from the "old" i
        break;
      case States.Init:
        process.emit('stopLogging');
        currentState = States.Init;
        i = 0;  // if other init tasks existed, would do them here too.
        currentState = States.Running;
        startLogger();
        break;
      default:
        res.status(400);
        break;
    }
    res.send();
  });

  app.listen(3000);
};
