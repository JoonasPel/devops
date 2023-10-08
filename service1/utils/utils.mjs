import axios from 'axios';

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
    return res.status;
  } catch (error) {
    console.log(error?.message);
  }
};
