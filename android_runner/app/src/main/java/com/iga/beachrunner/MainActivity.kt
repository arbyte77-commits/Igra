package com.iga.beachrunner

import android.graphics.Color
import android.os.Bundle
import android.view.Gravity
import android.view.View
import android.widget.Button
import android.widget.FrameLayout
import android.widget.LinearLayout
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity

class MainActivity : AppCompatActivity() {
    private lateinit var root: FrameLayout
    private lateinit var gameView: BeachRunnerView
    private lateinit var menuPanel: LinearLayout
    private lateinit var selectPanel: LinearLayout
    private lateinit var hudPanel: LinearLayout
    private lateinit var gameOverPanel: LinearLayout
    private lateinit var pausePanel: LinearLayout

    private lateinit var distanceText: TextView
    private lateinit var coinText: TextView
    private lateinit var scoreText: TextView
    private lateinit var overText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        root = FrameLayout(this)
        root.setBackgroundColor(Color.BLACK)

        gameView = BeachRunnerView(this) { stats ->
            distanceText.text = "DIST ${stats.distance}m"
            coinText.text = "COIN ${stats.coins}"
            scoreText.text = "SCORE ${stats.score}"
        }
        gameView.onGameOver = { finalScore, best ->
            overText.text = "Score $finalScore   Best $best"
            showPanel(gameOverPanel)
        }

        root.addView(gameView, FrameLayout.LayoutParams(-1, -1))
        buildMenu()
        buildCharacterSelect()
        buildHud()
        buildPause()
        buildGameOver()

        setContentView(root)
        showPanel(menuPanel)
    }

    private fun buildMenu() {
        menuPanel = makePanel()
        menuPanel.addView(makeTitle("A%A BEACH RUSH"))
        menuPanel.addView(makeSubtitle("Pixel Beach Endless Runner"))
        menuPanel.addView(makeButton("Play") { showPanel(selectPanel) })
        root.addView(menuPanel)
    }

    private fun buildCharacterSelect() {
        selectPanel = makePanel()
        selectPanel.addView(makeTitle("Select Runner"))
        val row = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
            gravity = Gravity.CENTER
        }
        row.addView(makeButton("Male") {
            gameView.selectCharacter(0)
            startGame()
        })
        row.addView(makeButton("Female") {
            gameView.selectCharacter(1)
            startGame()
        })
        selectPanel.addView(row)
        root.addView(selectPanel)
    }

    private fun buildHud() {
        hudPanel = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(24, 24, 24, 24)
            visibility = View.GONE
        }
        distanceText = hudText("DIST 0m")
        coinText = hudText("COIN 0")
        scoreText = hudText("SCORE 0")
        hudPanel.addView(distanceText)
        hudPanel.addView(coinText)
        hudPanel.addView(scoreText)

        val pauseBtn = makeSmallButton("Pause") {
            gameView.pauseGame()
            showPanel(pausePanel)
        }
        val pauseLp = FrameLayout.LayoutParams(-2, -2, Gravity.TOP or Gravity.END)
        pauseLp.topMargin = 24
        pauseLp.marginEnd = 24
        root.addView(pauseBtn, pauseLp)
        pauseBtn.tag = "pause_btn"
        pauseBtn.visibility = View.GONE

        val jumpBtn = makeSmallButton("JUMP") { gameView.jump() }
        val jumpLp = FrameLayout.LayoutParams(240, 120, Gravity.BOTTOM or Gravity.END)
        jumpLp.bottomMargin = 28
        jumpLp.marginEnd = 280
        root.addView(jumpBtn, jumpLp)
        jumpBtn.tag = "jump_btn"
        jumpBtn.visibility = View.GONE

        val slideBtn = makeSmallButton("SLIDE") { gameView.slide() }
        val slideLp = FrameLayout.LayoutParams(240, 120, Gravity.BOTTOM or Gravity.END)
        slideLp.bottomMargin = 28
        slideLp.marginEnd = 24
        root.addView(slideBtn, slideLp)
        slideBtn.tag = "slide_btn"
        slideBtn.visibility = View.GONE

        root.addView(hudPanel)
    }

    private fun buildPause() {
        pausePanel = makePanel()
        pausePanel.addView(makeTitle("Paused"))
        pausePanel.addView(makeButton("Resume") {
            gameView.resumeGame()
            showPanel(null)
            toggleHud(true)
        })
        pausePanel.addView(makeButton("Menu") {
            gameView.stopToMenu()
            showPanel(menuPanel)
        })
        root.addView(pausePanel)
    }

    private fun buildGameOver() {
        gameOverPanel = makePanel()
        gameOverPanel.addView(makeTitle("Game Over"))
        overText = makeSubtitle("Score 0")
        gameOverPanel.addView(overText)
        gameOverPanel.addView(makeButton("Restart") { startGame() })
        gameOverPanel.addView(makeButton("Menu") {
            gameView.stopToMenu()
            showPanel(menuPanel)
        })
        root.addView(gameOverPanel)
    }

    private fun startGame() {
        gameView.startGame()
        showPanel(null)
        toggleHud(true)
    }

    private fun showPanel(panel: View?) {
        menuPanel.visibility = if (panel === menuPanel) View.VISIBLE else View.GONE
        selectPanel.visibility = if (panel === selectPanel) View.VISIBLE else View.GONE
        pausePanel.visibility = if (panel === pausePanel) View.VISIBLE else View.GONE
        gameOverPanel.visibility = if (panel === gameOverPanel) View.VISIBLE else View.GONE
        if (panel != null) toggleHud(false)
    }

    private fun toggleHud(show: Boolean) {
        hudPanel.visibility = if (show) View.VISIBLE else View.GONE
        root.findViewWithTag<View>("pause_btn")?.visibility = if (show) View.VISIBLE else View.GONE
        root.findViewWithTag<View>("jump_btn")?.visibility = if (show) View.VISIBLE else View.GONE
        root.findViewWithTag<View>("slide_btn")?.visibility = if (show) View.VISIBLE else View.GONE
    }

    private fun makePanel(): LinearLayout = LinearLayout(this).apply {
        orientation = LinearLayout.VERTICAL
        gravity = Gravity.CENTER
        setBackgroundColor(Color.parseColor("#88000000"))
        visibility = View.GONE
        layoutParams = FrameLayout.LayoutParams(-1, -1)
    }

    private fun makeTitle(text: String): TextView = TextView(this).apply {
        this.text = text
        setTextColor(Color.WHITE)
        textSize = 32f
        setPadding(12, 12, 12, 12)
    }

    private fun makeSubtitle(text: String): TextView = TextView(this).apply {
        this.text = text
        setTextColor(Color.parseColor("#FFE7AB"))
        textSize = 20f
        setPadding(12, 12, 12, 24)
    }

    private fun makeButton(label: String, onClick: () -> Unit): Button = Button(this).apply {
        text = label
        textSize = 20f
        setPadding(40, 18, 40, 18)
        setOnClickListener { onClick() }
    }

    private fun makeSmallButton(label: String, onClick: () -> Unit): Button = Button(this).apply {
        text = label
        textSize = 16f
        setOnClickListener { onClick() }
    }

    private fun hudText(text: String): TextView = TextView(this).apply {
        this.text = text
        setTextColor(Color.parseColor("#1D1B1A"))
        textSize = 18f
    }

    override fun onResume() {
        super.onResume()
        gameView.resumeThread()
    }

    override fun onPause() {
        super.onPause()
        gameView.pauseThread()
    }
}
