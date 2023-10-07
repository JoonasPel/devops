import dotenv from 'dotenv';
import dns from 'dns';
import amqp from 'amqplib';
import { 
  writeToLogFile, 
  createOrClearLogFile, 
  sleep, 
  sendRequest }
from './utils/utils.mjs';

// configs
dotenv.config();
const service2name = process.env.service2ContainerName;
const rabbitmqName = process.env.rabbitmqContainerName;
const service2PORT = process.env.service2PORT;
const rabbitMessagesQueue = process.env.rabbitMessagesQueue;
const rabbitMessageTopic = process.env.rabbitMessageTopic;
const rabbitRoutingKey = process.env.rabbitMessagesRoutingKey;
let service2Address, channel, connection;

// starts rabbitmq connection
async function startRabbit() {
  try {
    const rabbitURL = "amqp://guest:guest@"+rabbitmqName+":5672";
    connection = await amqp.connect(rabbitURL);
    channel = await connection.createChannel();
    await channel.assertExchange(rabbitMessageTopic, 'topic', {durable: false});
    await channel.assertQueue(rabbitMessagesQueue, {durable: false});
    await channel.bindQueue(rabbitMessagesQueue, rabbitMessageTopic, rabbitRoutingKey);
    
    channel.publish(rabbitMessageTopic, 'anything.message',
      Buffer.from("Moi Joonas"));
    console.log("service1 connected to Rabbit");
  } catch (error) {
    console.error("service1 got error when trying to setup Rabbit: ", error);
    process.exit(1);
  }
};

await sleep(15000); // todo replace with wait-for-it.sh
await startRabbit();
// get address of service 2
dns.lookup(service2name, (error, address) => {
  if (!error) { service2Address = address; }
  //startLogger();
});

const startLogger = async () => {
  createOrClearLogFile();
  let i = 1;
  while (i<21){
    const date = new Date();
    let text = `${i} ${date.toISOString()} ${service2Address}:${service2PORT}`;
    writeToLogFile(text);
    await sendRequest(text, service2name, service2PORT);
    await sleep(2000);
    i++;
  }
  writeToLogFile('STOP');
  await sendRequest('STOP', service2name, service2PORT);
  process.exit(0);
};
