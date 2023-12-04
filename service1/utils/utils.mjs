import axios from 'axios';
import amqp from 'amqplib';
import dotenv from 'dotenv';


dotenv.config();
export const service2name = process.env.service2ContainerName;
export const service2Port = process.env.service2Port;
const URL = 'http://'+service2name+':'+service2Port;
const rabbitmqName = process.env.rabbitmqContainerName;
const rabbitMessagesQueue = process.env.rabbitMessagesQueue;
export const rabbitMessageTopic = process.env.rabbitMessageTopic;
const rabbitMessagesRoutingKey = process.env.rabbitMessagesRoutingKey;
const rabbitLogsQueue = process.env.rabbitLogsQueue;
export const rabbitLogTopic = process.env.rabbitLogTopic;
const rabbitLogsRoutingKey = process.env.rabbitLogsRoutingKey;

/**
 * @param {*} timeMS time to sleep as milliseconds
 */
export const sleep = (timeMS) => {
  return new Promise(resolve => setTimeout(resolve, timeMS));
};

export const sendRequest = async (text) => {
  try {
    const res = await axios.get(URL, {
      data: {
        text,
      },
      timeout: 5000,
    });
    return res.status;
  } catch (error) {
    console.log(error?.message);
  }
};

let channel, connection;
export const connectToRabbit = async () => {
  try {
    const rabbitURL = "amqp://guest:guest@"+rabbitmqName+":5672";
    connection = await amqp.connect(rabbitURL);
    channel = await connection.createChannel();
    console.log("service1 connected to Rabbit");
  } catch (error) {
    console.error("service1 got error when connecting Rabbit: ", error);
    process.exit(1);
  }
};

export const initRabbit = async () => {
  try {
    await channel.assertExchange(rabbitMessageTopic, 'topic', {durable: false});
    await channel.assertExchange(rabbitLogTopic, 'topic', {durable: false});
    await channel.assertQueue(rabbitMessagesQueue, {durable: false});
    await channel.assertQueue(rabbitLogsQueue, {durable: false});
    await channel.bindQueue(rabbitMessagesQueue, rabbitMessageTopic,
      rabbitMessagesRoutingKey);
    await channel.bindQueue(rabbitLogsQueue, rabbitLogTopic, rabbitLogsRoutingKey);
    console.log("service1 succesfully initialized Rabbitmq exchanges and queues");
  } catch (error) {
    console.error("service1 FAILED to initialize Rabbitmq exchanges and/or queues:", error);
    process.exit(1);
  }
};

export const sendToRabbit = (msg, topic, key) => {
  channel.publish(topic, key, Buffer.from(msg));
};

export const closeGracefully = () => {
  console.log("Trying to close service1 gracefully");
  connection.close();
  process.exit(0);
};
