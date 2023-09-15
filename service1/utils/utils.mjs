import fs from 'fs'
import path from 'path';
import axios from 'axios';

// file writing setups
const filename = new URL(import.meta.url).pathname;
const dirname = path.dirname(filename);
const filepath = path.join(dirname, '../logs/service1.log');

/**
 * Write text to log file and add newline
 * appendFileSync closes file automatically after writing
 * @param {*} text to write
 */
export const writeToLogFile = (text) => {
  const textWithNewline = text+'\n'
  try {
    fs.appendFileSync(filepath, textWithNewline);
  } catch (error) {
    console.error(error);
  }
};

/**
 * Creates the log file or if it exists, clears/wipes it.
 */
export const createOrClearLogFile = () => {
  try {
    fs.writeFileSync(filepath, '');
  } catch (error) {
    console.error(error);
  }
};

/**
 * @param {*} timeMS time to sleep as milliseconds
 */
export const sleep = (timeMS) => {
  return new Promise(resolve => setTimeout(resolve, timeMS));
};

/**
 * todo
 */
export const sendRequest = async (text, service2name, service2PORT) => {
  const URL = 'http://'+service2name+':'+service2PORT;
  try {
    const res = await axios.get(URL, {
      data: {
        text,
      },
      timeout: 5000,
    });
  } catch (error) {
    writeToLogFile(error?.message);
  }
};
