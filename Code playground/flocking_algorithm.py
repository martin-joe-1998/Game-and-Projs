import pygame
import random
import math

# initialize pygame
pygame.init()

# define screen size and color
WIDTH, HEIGHT = 800, 600
BLACK = (0,0,0)

# define flocking parameters
NUM_ENTITIES = 100
MAX_SPEED = 2
PERCEPTION_RADIUS = 50
SEPARATION_DISTANCE = 25

# create the Entity class
class Entity:
    def __init__(self, x, y):
        self.position = pygame.Vector2(x, y)
        self.velocity = pygame.Vector2(random.uniform(-1, 1), random.uniform(-1, 1))
        self.velocity.scale_to_length(MAX_SPEED)

    # implement entity behaviors: update the position and velocity of the entity basd on its neighbor's pos and vel
    def update(self, flock):
        # update the position and velocity based on flocking rules
        self.position += self.velocity
    
        self.wrap_edges()
    
        average_velocity = pygame.Vector2(0, 0)
        average_position = pygame.Vector2(0, 0)
        average_separation = pygame.Vector2(0, 0)
        num_neighbors = 0
    
        for other in flock:
            if other == self:
                continue
            
            distance = self.distance_to(other)
    
            if distance < PERCEPTION_RADIUS:
                average_velocity += other.velocity
                average_position += other.position
    
                if distance < SEPARATION_DISTANCE:
                    diff = self.position - other.position
                    diff.scale_to_length(1 / distance)
                    average_separation += diff
    
                num_neighbors += 1
        
        if num_neighbors > 0:
            average_velocity /= num_neighbors
            average_position /= num_neighbors
            average_separation /= num_neighbors
        
        self.velocity += average_velocity * 0.02
        self.velocity += average_position * 0.01
        self.velocity -= average_separation * 0.03
    
        self.velocity.scale_to_length(MAX_SPEED)
    
    # wrap the entitles around the screen so that they reappear on the opposite side when they reach the screen edges
    def wrap_edges(self):
        if self.position.x < 0:
            self.position.x = WIDTH
        if self.position.y < 0:
            self.position.y = HEIGHT
        if self.position.x > WIDTH:
            self.position.x = 0
        if self.position.y > HEIGHT:
            self.position.y = 0
    
    # calculate distance between entities
    def distance_to(self, other):
        return math.sqrt((self.position.x - other.position.x)**2 + (self.position.y - other.position.y)**2)

# create the flock, initialize NUM_ENTITIES flocks, each flock with random position
flock = [Entity(random.randint(0, WIDTH), random.randint(0, HEIGHT)) for _ in range(NUM_ENTITIES)]

# initialize the pygame screen
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Flocking Simulation")

# main game loop
def main():
    running = True
    clock = pygame.time.Clock()

    while running:
        # handle events
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
        
        # update entities and draw them on the screen
        screen.fill(BLACK)
        for entity in flock:
            entity.update(flock)
            pygame.draw.circle(screen, (255,255,255), (int(entity.position.x), int(entity.position.y)), 3)
        
        pygame.display.flip()
        clock.tick(60)
    
    pygame.quit()

if __name__ == '__main__':
    main()