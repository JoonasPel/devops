FROM debian
USER root
COPY . /home
WORKDIR /home
RUN apt-get update
RUN apt-get install -y nodejs
ENTRYPOINT node server.js
