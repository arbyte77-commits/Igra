package com.iga.beachrunner

import android.content.Context
import android.graphics.*
import android.util.Log
import android.view.SurfaceHolder
import android.view.SurfaceView
import java.io.IOException
import kotlin.math.max
import kotlin.random.Random

data class HudStats(val distance: Int, val coins: Int, val score: Int)

enum class Mode { MENU, PLAYING, PAUSED, GAME_OVER }

class BeachRunnerView(
    context: Context,
    private val onHud: (HudStats) -> Unit,
) : SurfaceView(context), Runnable {

    private var thread: Thread? = null
    private var running = false
    private var mode = Mode.MENU

    var onGameOver: ((Int, Int) -> Unit)? = null

    private val rnd = Random(System.currentTimeMillis())

    private var selectedCharacter = 0
    private var playerY = 820f
    private var velocityY = 0f
    private var sliding = false
    private var slideTimer = 0f
    private var shield = 0f

    private var runSpeed = 760f
    private var spawnTimer = 0f
    private var spawnGap = 1.1f

    private var distance = 0f
    private var coins = 0
    private var score = 0
    private var best = 0
    private val characterBitmaps = arrayOfNulls<Bitmap>(2)

    private val obstacles = mutableListOf<RectF>()
    private val pickups = mutableListOf<Pair<RectF, String>>()
    private val props = MutableList(16) { PointF((it * 170).toFloat(), (560 + rnd.nextInt(220)).toFloat()) }

    private val paint = Paint(Paint.ANTI_ALIAS_FLAG)
    private val playerPaint = Paint(Paint.ANTI_ALIAS_FLAG)

    init {
        loadCharacterBitmaps()
        isFocusable = true
        setOnTouchListener { _, e ->
            if (mode != Mode.PLAYING) return@setOnTouchListener true
            if (e.y < height * 0.5f) jump() else slide()
            true
        }
    }

    private fun loadCharacterBitmaps() {
        try {
            context.assets.open("custom_characters.png").use { input ->
                val sheet = BitmapFactory.decodeStream(input) ?: return
                val frameWidth = sheet.width / 2
                val frameHeight = sheet.height
                characterBitmaps[0] = Bitmap.createBitmap(sheet, 0, 0, frameWidth, frameHeight)
                characterBitmaps[1] = Bitmap.createBitmap(sheet, frameWidth, 0, frameWidth, frameHeight)
            }
        } catch (_: IOException) {
            Log.d("BeachRunnerView", "custom_characters.png not found in assets, using fallback pixel runners")
        }
    }

    fun selectCharacter(index: Int) {
        selectedCharacter = index.coerceIn(0, 1)
    }

    fun startGame() {
        distance = 0f
        coins = 0
        score = 0
        runSpeed = 760f
        spawnTimer = 0f
        spawnGap = 1.1f
        playerY = 820f
        velocityY = 0f
        shield = 0f
        sliding = false
        obstacles.clear()
        pickups.clear()
        mode = Mode.PLAYING
    }

    fun pauseGame() {
        if (mode == Mode.PLAYING) mode = Mode.PAUSED
    }

    fun resumeGame() {
        if (mode == Mode.PAUSED) mode = Mode.PLAYING
    }

    fun stopToMenu() {
        mode = Mode.MENU
    }

    fun jump() {
        if (mode == Mode.PLAYING && playerY >= 820f) velocityY = -1450f
    }

    fun slide() {
        if (mode == Mode.PLAYING && playerY >= 820f) {
            sliding = true
            slideTimer = 0.5f
        }
    }

    override fun run() {
        var last = System.nanoTime()
        while (running) {
            if (!holder.surface.isValid) continue
            val now = System.nanoTime()
            val dt = ((now - last) / 1_000_000_000f).coerceAtMost(0.033f)
            last = now

            update(dt)
            drawFrame(holder)
        }
    }

    private fun update(dt: Float) {
        if (mode != Mode.PLAYING) return

        runSpeed += 18f * dt
        distance += runSpeed * 0.07f * dt
        score = distance.toInt() + coins * 12
        onHud(HudStats(distance.toInt(), coins, score))

        velocityY += 3600f * dt
        playerY += velocityY * dt
        if (playerY > 820f) {
            playerY = 820f
            velocityY = 0f
        }

        if (sliding) {
            slideTimer -= dt
            if (slideTimer <= 0f) sliding = false
        }

        if (shield > 0f) shield -= dt

        spawnTimer -= dt
        spawnGap = max(0.42f, spawnGap - 0.01f * dt)
        if (spawnTimer <= 0f) {
            spawnTimer = spawnGap
            if (rnd.nextFloat() < 0.34f) {
                val y = 760f - rnd.nextInt(200)
                pickups += RectF(width + 80f, y, width + 130f, y + 50f) to listOf("coin", "coin", "coin", "shield", "speed")[rnd.nextInt(5)]
            } else {
                obstacles += RectF(width + 90f, 740f, width + 180f, 820f)
            }
        }

        val player = playerRect()

        for (i in obstacles.indices.reversed()) {
            val r = obstacles[i]
            r.offset(-runSpeed * dt, 0f)
            if (r.right < -20f) {
                obstacles.removeAt(i)
                continue
            }
            if (RectF.intersects(r, player)) {
                if (shield > 0f) {
                    shield = 0f
                    obstacles.removeAt(i)
                } else {
                    best = max(best, score)
                    mode = Mode.GAME_OVER
                    onGameOver?.invoke(score, best)
                    return
                }
            }
        }

        for (i in pickups.indices.reversed()) {
            val p = pickups[i]
            p.first.offset(-runSpeed * dt, 0f)
            if (p.first.right < -20f) {
                pickups.removeAt(i)
                continue
            }
            if (RectF.intersects(p.first, player)) {
                when (p.second) {
                    "coin" -> coins += 1
                    "shield" -> shield = 5.5f
                    "speed" -> runSpeed += 160f
                }
                pickups.removeAt(i)
            }
        }

        for (prop in props) {
            prop.x -= runSpeed * 0.18f * dt
            if (prop.x < -70f) {
                prop.x = width + rnd.nextInt(220)
                prop.y = (540 + rnd.nextInt(240)).toFloat()
            }
        }
    }

    private fun drawFrame(holder: SurfaceHolder) {
        val c = holder.lockCanvas() ?: return

        c.drawColor(Color.parseColor("#6FD3FF"))

        paint.color = Color.parseColor("#3DA2D6")
        c.drawRect(0f, 430f, width.toFloat(), 730f, paint)
        paint.color = Color.parseColor("#E9CB86")
        c.drawRect(0f, 730f, width.toFloat(), height.toFloat(), paint)

        for (prop in props) {
            paint.color = listOf(Color.parseColor("#6D8D48"), Color.parseColor("#D8614F"), Color.parseColor("#F6F3D7"))[((prop.x / 100).toInt().absoluteValue) % 3]
            c.drawRect(prop.x, prop.y - 120f, prop.x + 24f, prop.y, paint)
        }

        for (ob in obstacles) {
            paint.color = Color.parseColor("#60565A")
            c.drawRoundRect(ob, 12f, 12f, paint)
        }

        for (p in pickups) {
            paint.color = when (p.second) {
                "coin" -> Color.parseColor("#FFD24D")
                "shield" -> Color.parseColor("#58F0FF")
                else -> Color.parseColor("#92FF61")
            }
            c.drawOval(p.first, paint)
        }

        val pr = playerRect()
        val bitmap = characterBitmaps[selectedCharacter]
        if (bitmap != null) {
            val dst = RectF(pr.left - 12f, pr.top - 46f, pr.right + 12f, pr.bottom + 2f)
            val bitmapPaint = Paint().apply { isFilterBitmap = false }
            c.drawBitmap(bitmap, null, dst, bitmapPaint)
        } else {
            playerPaint.color = if (selectedCharacter == 0) Color.parseColor("#2F64FF") else Color.parseColor("#FF4FB2")
            c.drawRect(pr, playerPaint)
            playerPaint.color = Color.parseColor("#F8D8B3")
            c.drawRect(pr.left + 16f, pr.top - 26f, pr.right - 16f, pr.top, playerPaint)
        }

        if (shield > 0f) {
            paint.style = Paint.Style.STROKE
            paint.strokeWidth = 8f
            paint.color = Color.parseColor("#58F0FF")
            c.drawCircle((pr.left + pr.right) / 2f, (pr.top + pr.bottom) / 2f, 70f, paint)
            paint.style = Paint.Style.FILL
        }

        holder.unlockCanvasAndPost(c)
    }

    private fun playerRect(): RectF {
        val h = if (sliding) 64f else 112f
        return RectF(320f, playerY - h, 392f, playerY)
    }

    fun resumeThread() {
        if (running) return
        running = true
        thread = Thread(this)
        thread?.start()
    }

    fun pauseThread() {
        running = false
        thread?.join(300)
    }
}

private val Int.absoluteValue: Int
    get() = if (this < 0) -this else this
