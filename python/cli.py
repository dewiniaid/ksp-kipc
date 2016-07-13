import krpc
import code

conn = krpc.connect(name="KIPC Test Script")
print(conn.krpc.get_status())

processors = conn.kipc.get_processors(conn.space_center.active_vessel)
print(repr(processors))
part = processors[0].part
print(repr(part))
processor = conn.kipc.get_processor(part)
print(repr(processor))

code.interact(local=globals())

