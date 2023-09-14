function sleep(timeMS) {
  return new Promise(resolve => setTimeout(resolve, timeMS));
} 

let i = -1;
while (i<5){
  i++;
  console.log("Moi", i);
  await sleep(1000);
}
