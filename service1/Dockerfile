FROM node:20-alpine
WORKDIR /app
COPY package*.json .
COPY . /app

RUN apk add --no-cache bash
RUN wget -O /bin/wait-for-it.sh https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh
RUN chmod +x /bin/wait-for-it.sh

RUN npm ci
CMD [ "node", "index.mjs" ]