# build stage
FROM golang:1.17.5  AS build-env

RUN apt-get update && apt-get install -y \
    vim \
    procps 
RUN mkdir -p /go/src/github.com/eds-webhook
WORKDIR /go/src/github.com/eds-webhook
COPY  . .

RUN useradd -u 10001 webhook
#RUN go mod init
#RUN go env -w GOPRIVATE=github.com/VerizonDev/*

#ENV GOPRIVATE=github.com/VerizonDev/*
#RUN go mod vendor
#RUN go mod download
RUN CGO_ENABLED=0 GOOS=linux go build -a -ldflags '-extldflags "-static"' -o webhook

# FROM scratch

# COPY --from=build-env /go/src/github.com/eds-webhook/webhook .
# COPY --from=build-env /etc/passwd /etc/passwd
USER root
ENTRYPOINT ["./webhook"]