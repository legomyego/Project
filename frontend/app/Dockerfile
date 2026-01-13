# Dockerfile for Nuxt 3 Main Application
# Multi-stage build for optimized production image

# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
# This layer is cached unless dependencies change
COPY package.json package-lock.json ./
RUN npm ci

# Copy source code
COPY . .

# Build Nuxt app for production
# This generates .output directory with standalone server
RUN npm run build

# Stage 2: Production
FROM node:20-alpine AS production
WORKDIR /app

# Copy only the built output from build stage
# .output contains everything needed to run the app
COPY --from=build /app/.output ./.output

# Expose port 3000 (Nuxt default)
EXPOSE 3000

# Set environment to production
ENV NODE_ENV=production
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Run the Nuxt server
CMD ["node", ".output/server/index.mjs"]
