import axios from "axios";

const sleep = (timeMS) => {
  return new Promise(resolve => setTimeout(resolve, timeMS));
};

const send = async (index) => {
  try {
    const res = await axios.get('http://service2joonaspelttari:3001', {
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
let i = -1;
while (i<5){
  i++;
  await sleep(3000);
  await send(i);
}
console.log("service 1 closing");
