FROM nginx:1

COPY ./nginx/production/nginx.conf /etc/nginx/nginx.conf
COPY ./nginx/production/api_gateway.conf /etc/nginx/api_gateway.conf
COPY ./nginx/production/api_conf.d/ /etc/nginx/api_conf.d/

EXPOSE 443
