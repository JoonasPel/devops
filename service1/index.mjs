import dns from 'dns';
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


await sleep(15000); // todo replace with wait-for-it.sh
await connectToRabbit();
await initRabbit();
process.on('SIGTERM', closeGracefully);
let service2Address;
dns.lookup(service2name, (error, address) => {
  if (!error) { service2Address = address; }
  startLogger();
});

const startLogger = async () => {
  let i = 1;
  while (i<21){
    const date = new Date();
    const text = `SND ${i} ${date.toISOString()} ${service2Address}:${service2Port}`;
    sendToRabbit(text, rabbitMessageTopic, 'hi.message');
    const statusCode = await sendRequest(text) ?? 500;
    const newText = `${statusCode.toString()} ${new Date().toISOString()}`;
    sendToRabbit(newText, rabbitLogTopic, 'hi.log');
    await sleep(2000);
    i++;
  }
  sendToRabbit('SND STOP', rabbitLogTopic, 'hi.log');
};
