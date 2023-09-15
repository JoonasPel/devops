const fs = require('fs');
const path = require('path');

// file writing setups
const filepath = path.join(__dirname, '../logs/service2.log');

/**
 * Write text to log file and add newline
 * appendFileSync closes file automatically after writing
 * @param {*} text to write
 */
const writeToLogFile = (text) => {
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
const createOrClearLogFile = () => {
  try {
    fs.writeFileSync(filepath, '');
  } catch (error) {
    console.error(error);
  }
};

module.exports = {
  writeToLogFile,
  createOrClearLogFile,
};