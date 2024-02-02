DOCKER_COMPOSE = docker compose
DOCKER_COMPOSE_FILE = inf/docker-compose.yml

.PHONY: all
all: build up

.PHONY: up
up:
	${DOCKER_COMPOSE} --file ${DOCKER_COMPOSE_FILE} up

.PHONY: build
build:
	${DOCKER_COMPOSE} --file ${DOCKER_COMPOSE_FILE} build

.PHONY: clean
clean: down
	${DOCKER_COMPOSE} --file ${DOCKER_COMPOSE_FILE} down --volumes --remove-orphans

.PHONY: down
down:
	${DOCKER_COMPOSE} --file ${DOCKER_COMPOSE_FILE} down
