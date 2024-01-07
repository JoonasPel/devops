![architecture](https://github.com/JoonasPel/devops/blob/project/architecture.PNG?raw=true)

### API:
###### GET /messages (as text/plain)
Returns all message registered with Monitor-service. \
Example response (part of): \
SND 1 2022-10-01T06:35:01.373Z 192.168.2.22:8000 192.168.2.21:78390

###### PUT /state (payload “INIT”, “PAUSED”, “RUNNING”, “SHUTDOWN”)

PAUSED = Service 1 does not send messages \
RUNNING = Service 2 sends messages \
If the new state is equal to previous nothing happens. \
There are two special cases: \
INIT = everything (except log information for /run-log and /messages) is set to \
the initial state and Service 1 starts sending again, and state is set to RUNNING \
SHUTDOWN = all containers are stopped

###### GET /state (as text/plain)
Get the value of state.

##### GET /run-log (as text/plain)
Get information about state changes \
Example response: \
2023-11-01T06.35:01.380Z: INIT->RUNNING \
2023-11-01T06:40:01.373Z: RUNNING->PAUSED \
2023-11-01T06:40:01.373Z: PAUSET->RUNNING \

##### GET /mqstatistic 
Return core overall statistics of the RabbitMQ. \
And in addition for each queue, return the following: \
“message delivery rate”, \
“messages publishing rate”, \
“messages delivered recently”, \
“message published lately"