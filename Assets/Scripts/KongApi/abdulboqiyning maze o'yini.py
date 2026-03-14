import pygame
import sys

pygame.init()

# Oynaning o'lchamlari
WIDTH, HEIGHT = 600, 300
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Simple Geometry Dash")

clock = pygame.time.Clock()

# Ranglar
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
RED = (255, 0, 0)

# O'yinchi kvadrati
player_size = 40
player_x = 50
player_y = HEIGHT - player_size - 50
player_vel_y = 0
gravity = 1
jump_power = -15
is_jumping = False

# To'siqlar (blocklar)
obstacle_width = 30
obstacle_height = 60
obstacle_x = WIDTH
obstacle_y = HEIGHT - obstacle_height - 50
obstacle_speed = 7

font = pygame.font.SysFont(None, 48)
score = 0

def draw_player(x, y):
    pygame.draw.rect(screen, RED, (x, y, player_size, player_size))

def draw_obstacle(x, y):
    pygame.draw.rect(screen, BLACK, (x, y, obstacle_width, obstacle_height))

def show_score(scr):
    text = font.render(f"Score: {scr}", True, BLACK)
    screen.blit(text, (10, 10))

# O'yin sikli
running = True
while running:
    clock.tick(60)  # 60 FPS
    screen.fill(WHITE)

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

        # Sakrash tugmasi (bo'sh joy)
        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_SPACE and not is_jumping:
                player_vel_y = jump_power
                is_jumping = True

    # O'yinchi fizikasi
    player_vel_y += gravity
    player_y += player_vel_y
    if player_y >= HEIGHT - player_size - 50:
        player_y = HEIGHT - player_size - 50
        is_jumping = False

    # To'siq harakati
    obstacle_x -= obstacle_speed
    if obstacle_x < -obstacle_width:
        obstacle_x = WIDTH
        score += 1  # Ball qo'shish

    # To'qnashuvni tekshirish
    player_rect = pygame.Rect(player_x, player_y, player_size, player_size)
    obstacle_rect = pygame.Rect(obstacle_x, obstacle_y, obstacle_width, obstacle_height)

    if player_rect.colliderect(obstacle_rect):
        # O'yin tugadi
        running = False

    draw_player(player_x, player_y)
    draw_obstacle(obstacle_x, obstacle_y)
    show_score(score)

    pygame.display.flip()

pygame.quit()
sys.exit()