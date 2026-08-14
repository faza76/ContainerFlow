# ContainerFlow Dashboard (Vue 3 + Vite) — build context is repo root.
# Source: frontend/container-flow-dashboard/  (created by the Frontend agent)
# Produces an nginx image serving the built SPA on port 80.

FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies first for layer caching.
COPY frontend/container-flow-dashboard/package*.json ./
RUN npm ci || npm install

# Copy the app source and build.
COPY frontend/container-flow-dashboard/ ./
ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:1.27-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]