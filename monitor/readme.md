listens topic “log” of the message broker and keeps the received messages in the
memory. Monitor also listens GET requests in port 8087, and as a response returns the list of
received strings from the message broker – MIME-type “text/plain” and each message on a
separate line.