FROM node:20-alpine
WORKDIR /app
COPY package*.json .
COPY . /app
RUN npm ci
CMD [ "node", "index.js" ]