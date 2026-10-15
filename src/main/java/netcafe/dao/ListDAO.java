package netcafe.dao;

import netcafe.Identifiable;
import netcafe.model.Checks;
import java.util.*;
import java.util.stream.*;

/** Read-only access to the same objects and backing list maintained by services. */
public abstract class ListDAO<T extends Identifiable> {
    protected final List<T> values;

    protected ListDAO(List<T> values) {
        this.values = view(values);
    }

    public Optional<T> findById(String id) {
        String key = Checks.id(id);
        return values.stream().filter(value -> value.getId().equals(key)).findFirst();
    }

    /** A live, unmodifiable view: later service updates are visible. */
    public List<T> findAll() { return values; }

    protected static <E> List<E> view(List<E> list) {
        return Collections.unmodifiableList(Checks.required(list, "list"));
    }

    /** Query results are unmodifiable snapshots; entity references are preserved. */
    protected static <E> List<E> result(Stream<E> stream) {
        return Collections.unmodifiableList(stream.collect(Collectors.toList()));
    }
}
