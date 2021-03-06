#include "Sprites.hpp"
#include "ScriptError.hpp"
#include <algorithm>
namespace S2VX {
	const SpriteUniquePointerComparison Sprites::spriteComparison;
	void Sprites::draw() {
		for (const auto active : actives) {
			sprites[active]->draw();
		}
	}
	void Sprites::addSprite(std::unique_ptr<Sprite>&& sprite) {
		sprites.push_back(std::move(sprite));
	}
	Sprite* const Sprites::getLastSprite() {
		if (sprites.empty()) {
			throw ScriptError("SpriteBind must be called before other sprite command.");
		}
		return sprites.back().get();
	}
	void Sprites::update(const int time) {
		while (nextActive != sprites.size() && sprites[nextActive]->getStart() <= time) {
			actives.insert(nextActive++);
		}
		for (const auto active : actives) {
			sprites[active]->update(time);
		}
		for (auto active = actives.begin(); active != actives.end(); ) {
			if (sprites[*active]->getEnd() <= time) {
				active = actives.erase(active);
			}
			else {
				++active;
			}
		}
	}
	void Sprites::sort() {
		for (auto& sprite : sprites) {
			sprite->sort();
		}
		std::sort(sprites.begin(), sprites.end(), spriteComparison);
	}
}