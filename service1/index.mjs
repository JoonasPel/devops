import axios from 'axios';
import fs from 'fs'
import path from 'path';
import dns from 'dns';

const service2name = 'service2joonaspelttari';
const service2PORT = 8000;
let service2Address;

// get address of service 2
dns.lookup(service2name, (error, address) => {
  if (error) {
    console.error('Error getting service2 address:', error);
  } else {
    service2Address = address;
  }
});

// file writing setups
const filename = new URL(import.meta.url).pathname;
const dirname = path.dirname(filename);
const filepath = path.join(dirname, '/logs/testi.txt');
console.log("PATHIII:", filepath);

const writeToLogFile = (text) => {
  const textWithNewline = text+'\n'
  try {
    fs.appendFileSync(filepath, textWithNewline);
  } catch (error) {
    console.error(error);
  }
}

const sleep = (timeMS) => {
  return new Promise(resolve => setTimeout(resolve, timeMS));
};

const send = async (index) => {
  const URL = 'http://'+service2name+':'+service2PORT;
  try {
    const res = await axios.get(URL, {
      data: {
        moi: "Joonas!!!!",
        index,
      },
      timeout: 5000,
    });
  } catch (error) {
    console.log(error)
  }
};

console.log("service 1 running");
let i = 1;
while (i<5){
  const date = new Date();
  let text = i+' '+date.toISOString();
  writeToLogFile(text);
  await sleep(2000);
  await send(i);

  i++;
}
console.log("service 1 closing");
