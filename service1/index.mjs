import dotenv from 'dotenv';
import dns from 'dns';
import amqp from 'amqplib';
import {
  sleep, 
  sendRequest }
from './utils/utils.mjs';

// configs
dotenv.config();
const service2name = process.env.service2ContainerName;
const rabbitmqName = process.env.rabbitmqContainerName;
const service2Port = process.env.service2Port;
const rabbitMessagesQueue = process.env.rabbitMessagesQueue;
const rabbitMessageTopic = process.env.rabbitMessageTopic;
const rabbitMessagesRoutingKey = process.env.rabbitMessagesRoutingKey;
const rabbitLogsQueue = process.env.rabbitLogsQueue;
const rabbitLogTopic = process.env.rabbitLogTopic;
const rabbitLogsRoutingKey = process.env.rabbitLogsRoutingKey;
let service2Address, channel, connection;

// starts rabbitmq connection
const startRabbit = async () => {
  try {
    const rabbitURL = "amqp://guest:guest@"+rabbitmqName+":5672";
    connection = await amqp.connect(rabbitURL);
    channel = await connection.createChannel();
    await channel.assertExchange(rabbitMessageTopic, 'topic', {durable: false});
    await channel.assertExchange(rabbitLogTopic, 'topic', {durable: false});
    await channel.assertQueue(rabbitMessagesQueue, {durable: false});
    await channel.assertQueue(rabbitLogsQueue, {durable: false});
    await channel.bindQueue(rabbitMessagesQueue, rabbitMessageTopic, rabbitMessagesRoutingKey);
    await channel.bindQueue(rabbitLogsQueue, rabbitLogTopic, rabbitLogsRoutingKey);
    console.log("service1 connected to Rabbit");
  } catch (error) {
    console.error("service1 got error when trying to setup Rabbit: ", error);
    process.exit(1);
  }
};

const sendToRabbit = (msg, topic, key) => {
  channel.publish(topic, key, Buffer.from(msg));
};

await sleep(15000); // todo replace with wait-for-it.sh
await startRabbit();
process.on('SIGTERM', () => {connection.close(); process.exit(0);});
dns.lookup(service2name, (error, address) => {
  if (!error) { service2Address = address; }
  startLogger();
});

const startLogger = async () => {
  let i = 1;
  while (i<21){
    const date = new Date();
    let text = `SND ${i} ${date.toISOString()} ${service2Address}:${service2Port}`;
    sendToRabbit(text, rabbitMessageTopic, 'hi.message');
    const statusCode = await sendRequest(text, service2name, service2Port) ?? -1;
    const newText = `${statusCode.toString()} ${new Date().toISOString()}`;
    sendToRabbit(newText, rabbitLogTopic, 'hi.log');
    await sleep(2000);
    i++;
  }
  sendToRabbit('SND STOP', rabbitLogTopic, 'hi.log');
};
