@everyone



**C#**



Please don't even think about using JSON to serialise items for _storage_ (you can pass items in JSON from the client to the server, of course). It's a bad idea. You will need to _edit_ inventories, and fields should be somehow preserved. E.g., it's possible to change the field title. Or remove the field. Of course, you shouldn't try to edit or "fix" items on the fly.



Think about this problem in this way: all the items for a given inventory should be "compatible," and you need to calculate aggregate values for them.



Also, don't even think about generating tables in the database on the fly. It's bad idea for several reasons. 



You need up to 3 fields of each type only. It means that you can consider the fields fixed and only manage whether they are shown and what titles are rendered. The relational database fits this task _perfectly_. It will work fast, and you won't get into trouble with "I don't know how to aggregate data from documents with different fields."



> And you _must_ use ORM in your project implementation.