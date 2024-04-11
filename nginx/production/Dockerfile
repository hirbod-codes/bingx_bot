FROM nginx:1

COPY ./nginx/development/nginx.conf /etc/nginx/nginx.conf
COPY ./nginx/development/api_gateway.conf /etc/nginx/api_gateway.conf
COPY -r ./nginx/development/api_conf.d/ /etc/nginx/api_conf.d/
