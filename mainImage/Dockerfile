FROM base

RUN apt-get install -y openssh-server
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config
RUN apt-get install -y net-tools
RUN useradd -m -s /bin/bash -G sudo -p $(openssl passwd -1 123456789moi) ssluser

RUN apt-get install -y python3
RUN apt-get install -y sudo
RUN apt-get install -y python3-apt

ENV PORT=8894
EXPOSE 22

ENTRYPOINT service ssh start && node server.js
