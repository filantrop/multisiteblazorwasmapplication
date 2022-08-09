FROM jwilder/nginx-proxy
RUN mkdir /etc/nginx/vhost.d
RUN echo "return 301 \$scheme://www.lifeplanner.nu\$request_uri;">/etc/nginx/vhost.d/lifeplanner.nu