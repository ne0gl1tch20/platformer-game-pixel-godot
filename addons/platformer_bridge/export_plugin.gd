@tool
extends EditorPlugin

var export_plugin: AndroidExportPlugin

func _enter_tree():
    export_plugin = AndroidExportPlugin.new()
    add_export_plugin(export_plugin)

func _exit_tree():
    remove_export_plugin(export_plugin)
    export_plugin = null

class AndroidExportPlugin extends EditorExportPlugin:
    var _plugin_name = "PlatformerBridge"

    func _supports_platform(platform):
        return platform is EditorExportPlatformAndroid

    func _get_android_libraries(platform, debug):
        var path = "res://addons/platformer_bridge/bin/debug/platformer_bridge-debug.aar" if debug else "res://addons/platformer_bridge/bin/release/platformer_bridge-release.aar"
        return PackedStringArray([path]) if FileAccess.file_exists(path) else PackedStringArray([])

    func _get_name():
        return _plugin_name
