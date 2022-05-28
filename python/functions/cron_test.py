
counter = 0

def handle(event, context):

  if counter > 1000:
    counter = 0
  else:
    counter = counter + 1

  print(f"this is a test. {counter}")

