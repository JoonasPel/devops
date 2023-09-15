import dns from 'dns';
import { 
  writeToLogFile, 
  createOrClearLogFile, 
  sleep, 
  send }
from './utils/utils.mjs';

// configs
const service2name = 'service2joonaspelttari';
const service2PORT = 8000;
let service2Address;

// get address of service 2
dns.lookup(service2name, (error, address) => {
  if (!error) { service2Address = address; }
  startLogger();
});

const startLogger = async () => {
  createOrClearLogFile();
  let i = 1;
  while (i<8){
    const date = new Date();
    let text = `${i} ${date.toISOString()} ${service2Address}:${service2PORT}`;
    writeToLogFile(text);
    await sleep(2000);
    await send(i, service2name, service2PORT);
    i++;
  }
};
