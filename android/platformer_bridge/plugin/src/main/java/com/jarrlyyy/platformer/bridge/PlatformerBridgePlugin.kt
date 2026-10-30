package com.jarrlyyy.platformer.bridge

import android.os.VibrationEffect
import android.os.Vibrator
import android.widget.Toast
import org.godotengine.godot.Godot
import org.godotengine.godot.plugin.GodotPlugin
import org.godotengine.godot.plugin.UsedByGodot

class PlatformerBridgePlugin(godot: Godot) : GodotPlugin(godot) {
    override fun getPluginName() = "PlatformerBridge"

    @UsedByGodot
    fun vibrate(milliseconds: Long) {
        runOnHostThread {
            val vibrator = activity?.getSystemService(Vibrator::class.java) ?: return@runOnHostThread
            if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                vibrator.vibrate(VibrationEffect.createOneShot(milliseconds, VibrationEffect.DEFAULT_AMPLITUDE))
            } else {
                @Suppress("DEPRECATION")
                vibrator.vibrate(milliseconds)
            }
        }
    }

    @UsedByGodot
    fun toast(message: String) {
        runOnHostThread {
            Toast.makeText(activity, message, Toast.LENGTH_SHORT).show()
        }
    }
}
