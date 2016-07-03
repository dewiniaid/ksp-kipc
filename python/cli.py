import krpc
import code

conn = krpc.connect(name="KIPC Test Script")
print(conn.krpc.get_status())

code.interact(local=globals())
