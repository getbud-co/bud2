FROM node:20-alpine AS base

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source
COPY src ./src
COPY public ./public
COPY next.config.ts tsconfig.json ./

# Build
RUN npm run build

# Production image
FROM node:20-alpine AS production

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install only production dependencies
RUN npm ci --omit=dev

# Copy built app
COPY --from=base /app/.next ./.next
COPY --from=base /app/public ./public

EXPOSE 3000

ENV NODE_ENV production

CMD ["npm", "start"]
