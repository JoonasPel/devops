How to Test:

1. Go to ./baseImage and build it "docker build --tag base ."
2. Go to ./mainImage and build it "docker build --tag main ."
3. Run the first container "docker run --name container1 main"
4. Get the ip of the first container "docker exec container1 ifconfig" (eth0 -> inet)
5. Connect to the container with command: "ssh ssluser@172.17.0.2" (use the ip that you got in step 4)
6. Give password "123456789moi"
7. Give command "exit" and then go to ./ansible. Make sure the ip is in inventory.ini.
8. Run playbook with this command inside ./ansible:
ansible-playbook -i inventory.ini playbook.yaml --extra-vars "ansible_user=ssluser ansible_password=123456789moi ansible_sudo_pass=123456789moi"

-Ansible should be able to connect to the first container, install git there and print uptime. And also say that the other ip is unreachable.

