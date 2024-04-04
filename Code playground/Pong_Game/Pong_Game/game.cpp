#define is_down(b) input->buttons[b].is_down
#define pressed(b) (input->buttons[b].is_down && input->buttons[b].changed)
#define released(b) (!input->buttons[b].is_down && input->buttons[b].changed)

// dp is velocity, ddp is acceleration
float player_1_pos, player_1_dp, player_2_pos, player_2_dp;
float arena_half_size_x = 85, arena_half_size_y = 45;
float player_half_size_x = 2.5, player_half_size_y = 12;

float ball_pos_x, ball_pos_y, ball_dp_x = 100.0, ball_dp_y, ball_half_size = 1;

int player_1_score, player_2_score;

internal void simulate_player(float* p, float* dp, float ddp, float dt) {
	// a decay by current speed
	ddp -= *dp * 10.f;
	*p = *p + *dp * dt + ddp * dt * dt * .5f;
	*dp = *dp + ddp * dt;

	// Collision Dectect between up&down boundary and (player1 : rightside)
	if (*p + player_half_size_y > arena_half_size_y) {
		// stop at certain position
		*p = arena_half_size_y - player_half_size_y;
		// reset velocity
		*dp = 0;
	}
	else if (*p - player_half_size_y < -arena_half_size_y) {
		*p = -arena_half_size_y + player_half_size_y;
		*dp = 0;
	}
}

internal bool aabb_vs_aabb(float p1x, float p1y, float hs1x, float hs1y, float p2x, float p2y, float hs2x, float hs2y) {
	return (
		p1x + hs1x > p2x - hs2x &&
		p1x - hs1x < p2x + hs2x &&
		p1y + hs1y > p2y - hs2y &&
		p1y + hs1y < p2y + hs2y
		);
}

internal void simulate_game(Input* input, float dt)
{
	clear_screen(0xff5500);

	// background
	draw_rect(0, 0, arena_half_size_x, arena_half_size_y, 0xffaa33);

    // double derivative of pos (acceleration)
	float player_1_ddp = 0.f;
	
	// enemy AI
#if 0
	// change acceleration by up and down
	if (is_down(BUTTON_UP)) player_1_ddp += 1500;
	if (is_down(BUTTON_DOWN)) player_1_ddp -= 1500;
#else
	player_1_ddp = (ball_pos_y - player_1_pos) * 100;
	if (player_1_ddp > 1100) player_1_ddp = 1100;
	if (player_1_ddp < -1100) player_1_ddp = -1100;
#endif

	float player_2_ddp = 0.f;
	if (is_down(BUTTON_W)) player_2_ddp += 1500;
	if (is_down(BUTTON_S)) player_2_ddp -= 1500;

	simulate_player(&player_1_pos, &player_1_dp, player_1_ddp, dt);
	simulate_player(&player_2_pos, &player_2_dp, player_2_ddp, dt);

	// Simulate ball
	{
		ball_pos_x += ball_dp_x * dt;
		ball_pos_y += ball_dp_y * dt;

		// collision detection between ball and player1
		if (aabb_vs_aabb(ball_pos_x, ball_pos_y, ball_half_size, ball_half_size, 80, player_1_pos, player_half_size_x, player_half_size_y)) {
			ball_pos_x = 80 - player_half_size_x - ball_half_size;
			// rebound
			ball_dp_x *= -1;
			ball_dp_y = (ball_pos_y - player_1_pos) * 2 + player_1_dp * .75f;
		}
		// collision detection between ball and player2
		else if (aabb_vs_aabb(ball_pos_x, ball_pos_y, ball_half_size, ball_half_size, -80, player_2_pos, player_half_size_x, player_half_size_y)) {
			ball_pos_x = -80 + player_half_size_x + ball_half_size;
			ball_dp_x *= -1;
			ball_dp_y = (ball_pos_y - player_2_pos) * 2 + player_2_dp * .75f;
		}

		// collision detection between ball and boundary
		if (ball_pos_y + ball_half_size > arena_half_size_y) {
			ball_pos_y = arena_half_size_y - ball_half_size;
			ball_dp_y *= -1;
		}
		else if (ball_pos_y - ball_half_size < -arena_half_size_y) {
			ball_pos_y = -arena_half_size_y + ball_half_size;
			ball_dp_y *= -1;
		}

		// if ball is out of bound
		// rightside
		if (ball_pos_x + ball_half_size > arena_half_size_x) {
			ball_dp_x *= -1;
			ball_dp_y = 0;
			ball_pos_x = 0;
			ball_pos_y = 0;
			player_1_score++;
		}
		// leftside
		else if (ball_pos_x - ball_half_size < -arena_half_size_x) {
			ball_dp_x *= -1;
			ball_dp_y = 0;
			ball_pos_x = 0;
			ball_pos_y = 0;
			player_2_score++;
		}
	}

	draw_number(player_1_score, -10, 40, 1.f, 0xbbffbb);
	draw_number(player_2_score, 10, 40, 1.f, 0xbbffbb);

	// drawing object
	draw_rect(ball_pos_x, ball_pos_y, ball_half_size, ball_half_size, 0xffffff); // pong ball
	draw_rect(80, player_1_pos, player_half_size_x, player_half_size_y, 0xff0000); // right side player
	draw_rect(-80, player_2_pos, player_half_size_x, player_half_size_y, 0xff0000); // left side player
}