FROM nginx:1.17.7
COPY ./dist /app
COPY nginx.conf nginx.security.conf /etc/nginx/
EXPOSE 80
ENTRYPOINT ["nginx","-g","daemon off;"]