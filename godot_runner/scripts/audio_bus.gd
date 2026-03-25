extends Node
class_name AudioBus

var music_player := AudioStreamPlayer.new()
var sfx_player := AudioStreamPlayer.new()

func _ready() -> void:
	add_child(music_player)
	add_child(sfx_player)

func play_music(stream: AudioStream) -> void:
	music_player.stream = stream
	music_player.play()

func play_sfx(stream: AudioStream) -> void:
	sfx_player.stream = stream
	sfx_player.play()
