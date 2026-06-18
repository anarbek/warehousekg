# ─── Stage 1: Build Angular app ────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files first for layer caching
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm install --legacy-peer-deps

# Copy source code
COPY . .

# Build for production with i18n (ru-RU + ky-KG)
RUN npm run build -- --configuration production

# ─── Stage 2: Serve with nginx ─────────────────────────────────────────────
FROM nginx:alpine

# Remove default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Create SSL directory (certs mounted at runtime)
RUN mkdir -p /etc/nginx/ssl

# Copy built Angular app
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80 443

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget --no-check-certificate --quiet --spider https://127.0.0.1:443/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
